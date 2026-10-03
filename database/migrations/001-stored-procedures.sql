DROP PROCEDURE IF EXISTS sp_get_user_for_login;
DROP PROCEDURE IF EXISTS sp_create_show;
DROP PROCEDURE IF EXISTS sp_get_show;
DROP PROCEDURE IF EXISTS sp_create_reservation;
DROP PROCEDURE IF EXISTS sp_cancel_reservation;
DROP PROCEDURE IF EXISTS sp_get_available_seats_metrics;

DELIMITER $$

CREATE PROCEDURE sp_get_user_for_login(IN p_username VARCHAR(64))
BEGIN
    SELECT UserGuid, Username, PasswordHash, IsAdmin
    FROM Users
    WHERE Username = p_username
    LIMIT 1;
END$$

CREATE PROCEDURE sp_create_show(
    IN p_show_guid CHAR(36),
    IN p_name VARCHAR(128),
    IN p_price_paise BIGINT,
    IN p_per_user_limit INT
)
BEGIN
    DECLARE v_show_id BIGINT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    INSERT INTO Shows (ShowGuid, Name, PricePaise, PerUserLimit)
    VALUES (p_show_guid, p_name, p_price_paise, p_per_user_limit);

    SET v_show_id = LAST_INSERT_ID();

    INSERT INTO Seats (SeatGuid, ShowId, SeatNumber, StatusId)
    SELECT UUID(), v_show_id, SeatNumber, 1
    FROM tmp_show_seats
    ORDER BY SeatNumber;

    COMMIT;

    SELECT
        s.ShowGuid, s.Name, s.PricePaise, s.PerUserLimit,
        COUNT(se.Id) AS TotalSeats,
        SUM(CASE WHEN se.StatusId = 1 THEN 1 ELSE 0 END) AS AvailableSeats,
        SUM(CASE WHEN se.StatusId = 2 THEN 1 ELSE 0 END) AS HeldSeats,
        SUM(CASE WHEN se.StatusId = 3 THEN 1 ELSE 0 END) AS ConfirmedSeats
    FROM Shows s
    LEFT JOIN Seats se ON se.ShowId = s.Id
    WHERE s.Id = v_show_id
    GROUP BY s.Id;

    SELECT se.SeatGuid, se.SeatNumber, st.Name AS Status
    FROM Seats se
    JOIN EnumStatus st ON st.Id = se.StatusId
    WHERE se.ShowId = v_show_id
    ORDER BY se.Id;
END$$

CREATE PROCEDURE sp_get_show(IN p_show_guid CHAR(36))
BEGIN
    SELECT
        s.ShowGuid, s.Name, s.PricePaise, s.PerUserLimit,
        COUNT(se.Id) AS TotalSeats,
        SUM(CASE WHEN se.StatusId = 1 THEN 1 ELSE 0 END) AS AvailableSeats,
        SUM(CASE WHEN se.StatusId = 2 THEN 1 ELSE 0 END) AS HeldSeats,
        SUM(CASE WHEN se.StatusId = 3 THEN 1 ELSE 0 END) AS ConfirmedSeats
    FROM Shows s
    LEFT JOIN Seats se ON se.ShowId = s.Id
    WHERE s.ShowGuid = p_show_guid
    GROUP BY s.Id;

    SELECT se.SeatGuid, se.SeatNumber, st.Name AS Status
    FROM Seats se
    JOIN Shows s ON s.Id = se.ShowId
    JOIN EnumStatus st ON st.Id = se.StatusId
    WHERE s.ShowGuid = p_show_guid
    ORDER BY se.Id;
END$$

CREATE PROCEDURE sp_create_reservation(
    IN p_user_guid CHAR(36),
    IN p_show_guid CHAR(36),
    IN p_idempotency_key VARCHAR(128),
    IN p_request_hash CHAR(64),
    IN p_reservation_guid CHAR(36)
)
BEGIN
    DECLARE v_user_id BIGINT DEFAULT NULL;
    DECLARE v_show_id BIGINT DEFAULT NULL;
    DECLARE v_price_paise BIGINT DEFAULT 0;
    DECLARE v_per_user_limit INT DEFAULT 4;
    DECLARE v_existing_id BIGINT DEFAULT NULL;
    DECLARE v_existing_hash CHAR(64);
    DECLARE v_reservation_id BIGINT DEFAULT NULL;
    DECLARE v_requested_count INT DEFAULT 0;
    DECLARE v_found_count INT DEFAULT 0;
    DECLARE v_unavailable_count INT DEFAULT 0;
    DECLARE v_existing_count INT DEFAULT 0;
    DECLARE v_seat_id BIGINT;
    DECLARE v_seat_status INT;
    DECLARE v_done INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    SELECT Id INTO v_user_id
    FROM Users
    WHERE UserGuid = p_user_guid
    FOR UPDATE;

    IF v_user_id IS NULL THEN
        ROLLBACK;
        SELECT 'USER_NOT_FOUND' AS Outcome;
    ELSE
        SELECT Id, PricePaise, PerUserLimit
        INTO v_show_id, v_price_paise, v_per_user_limit
        FROM Shows
        WHERE ShowGuid = p_show_guid
        LIMIT 1;

        IF v_show_id IS NULL THEN
            ROLLBACK;
            SELECT 'SHOW_NOT_FOUND' AS Outcome;
        ELSE
            SELECT Id, RequestHash
            INTO v_existing_id, v_existing_hash
            FROM Reservations
            WHERE UserId = v_user_id
              AND IdempotencyKey = p_idempotency_key
            LIMIT 1;

            IF v_existing_id IS NOT NULL THEN
                IF v_existing_hash <> p_request_hash THEN
                    ROLLBACK;
                    SELECT 'IDEMPOTENCY_CONFLICT' AS Outcome;
                ELSE
                    COMMIT;

                    SELECT
                        'IDEMPOTENT_REPLAY' AS Outcome,
                        r.ReservationGuid,
                        s.ShowGuid,
                        u.UserGuid,
                        COUNT(rs.SeatId) * s.PricePaise AS AmountPaise,
                        st.Name AS Status,
                        se.SeatNumber
                    FROM Reservations r
                    JOIN Users u ON u.Id = r.UserId
                    JOIN Shows s ON s.Id = r.ShowId
                    JOIN EnumStatus st ON st.Id = r.StatusId
                    JOIN ReservationSeats rs ON rs.ReservationId = r.Id
                    JOIN Seats se ON se.Id = rs.SeatId
                    WHERE r.Id = v_existing_id
                    GROUP BY r.Id, r.ReservationGuid, s.ShowGuid, u.UserGuid, s.PricePaise, st.Name, se.Id, se.SeatNumber
                    ORDER BY se.Id;
                END IF;
            ELSE
                SELECT COUNT(*) INTO v_requested_count
                FROM tmp_reservation_seats;

                SELECT COUNT(*) INTO v_existing_count
                FROM ReservationSeats rs
                JOIN Reservations r ON r.Id = rs.ReservationId
                WHERE r.UserId = v_user_id
                  AND r.ShowId = v_show_id
                  AND r.StatusId = 3;

                IF v_existing_count + v_requested_count > v_per_user_limit THEN
                    ROLLBACK;
                    SELECT 'PER_USER_LIMIT' AS Outcome;
                ELSE
                    BEGIN
                        DECLARE seat_cursor CURSOR FOR
                            SELECT se.Id, se.StatusId
                            FROM Seats se
                            JOIN tmp_reservation_seats requested
                              ON requested.SeatNumber = se.SeatNumber
                            WHERE se.ShowId = v_show_id
                            ORDER BY se.Id
                            FOR UPDATE;
                        DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_done = 1;

                        OPEN seat_cursor;

                        read_seats: LOOP
                            FETCH seat_cursor INTO v_seat_id, v_seat_status;

                            IF v_done = 1 THEN
                                LEAVE read_seats;
                            END IF;

                            SET v_found_count = v_found_count + 1;

                            IF v_seat_status <> 1 THEN
                                SET v_unavailable_count = v_unavailable_count + 1;
                            END IF;
                        END LOOP;

                        CLOSE seat_cursor;
                    END;

                    IF v_found_count <> v_requested_count THEN
                        ROLLBACK;
                        SELECT 'INVALID_SEAT' AS Outcome;
                    ELSEIF v_unavailable_count > 0 THEN
                        ROLLBACK;
                        SELECT 'SEAT_TAKEN' AS Outcome;
                    ELSE
                        INSERT INTO Reservations (
                            ReservationGuid, UserId, ShowId, StatusId,
                            IdempotencyKey, RequestHash
                        )
                        VALUES (
                            p_reservation_guid, v_user_id, v_show_id, 3,
                            p_idempotency_key, p_request_hash
                        );

                        SET v_reservation_id = LAST_INSERT_ID();

                        UPDATE Seats se
                        JOIN tmp_reservation_seats requested
                          ON requested.SeatNumber = se.SeatNumber
                        SET se.StatusId = 3
                        WHERE se.ShowId = v_show_id;

                        INSERT INTO ReservationSeats (ReservationId, SeatId)
                        SELECT v_reservation_id, se.Id
                        FROM Seats se
                        JOIN tmp_reservation_seats requested
                          ON requested.SeatNumber = se.SeatNumber
                        WHERE se.ShowId = v_show_id
                        ORDER BY se.Id;

                        COMMIT;

                        SELECT
                            'CONFIRMED' AS Outcome,
                            r.ReservationGuid,
                            s.ShowGuid,
                            u.UserGuid,
                            v_requested_count * s.PricePaise AS AmountPaise,
                            st.Name AS Status,
                            se.SeatNumber
                        FROM Reservations r
                        JOIN Users u ON u.Id = r.UserId
                        JOIN Shows s ON s.Id = r.ShowId
                        JOIN EnumStatus st ON st.Id = r.StatusId
                        JOIN ReservationSeats rs ON rs.ReservationId = r.Id
                        JOIN Seats se ON se.Id = rs.SeatId
                        WHERE r.Id = v_reservation_id
                        ORDER BY se.Id;
                    END IF;
                END IF;
            END IF;
        END IF;
    END IF;
END$$

CREATE PROCEDURE sp_cancel_reservation(
    IN p_user_guid CHAR(36),
    IN p_reservation_guid CHAR(36)
)
BEGIN
    DECLARE v_user_id BIGINT DEFAULT NULL;
    DECLARE v_reservation_id BIGINT DEFAULT NULL;
    DECLARE v_status_id INT DEFAULT NULL;
    DECLARE v_seat_count INT DEFAULT 0;
    DECLARE v_updated_count INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    SELECT Id INTO v_user_id
    FROM Users
    WHERE UserGuid = p_user_guid
    FOR UPDATE;

    IF v_user_id IS NULL THEN
        ROLLBACK;
        SELECT 'NOT_FOUND' AS Outcome;
    ELSE
        SELECT Id, StatusId
        INTO v_reservation_id, v_status_id
        FROM Reservations
        WHERE ReservationGuid = p_reservation_guid
          AND UserId = v_user_id
        FOR UPDATE;

        IF v_reservation_id IS NULL THEN
            ROLLBACK;
            SELECT 'NOT_FOUND' AS Outcome;
        ELSEIF v_status_id = 4 THEN
            COMMIT;

            SELECT
                'CANCELLED' AS Outcome,
                r.ReservationGuid, s.ShowGuid, u.UserGuid,
                COUNT(rs.SeatId) * s.PricePaise AS AmountPaise,
                st.Name AS Status, se.SeatNumber
            FROM Reservations r
            JOIN Users u ON u.Id = r.UserId
            JOIN Shows s ON s.Id = r.ShowId
            JOIN EnumStatus st ON st.Id = r.StatusId
            JOIN ReservationSeats rs ON rs.ReservationId = r.Id
            JOIN Seats se ON se.Id = rs.SeatId
            WHERE r.Id = v_reservation_id
            GROUP BY r.Id, r.ReservationGuid, s.ShowGuid, u.UserGuid, s.PricePaise, st.Name, se.Id, se.SeatNumber
            ORDER BY se.Id;
        ELSE
            SELECT COUNT(*) INTO v_seat_count
            FROM ReservationSeats
            WHERE ReservationId = v_reservation_id;

            UPDATE Seats se
            JOIN ReservationSeats rs ON rs.SeatId = se.Id
            SET se.StatusId = 1
            WHERE rs.ReservationId = v_reservation_id
              AND se.StatusId = 3;

            SET v_updated_count = ROW_COUNT();

            IF v_updated_count <> v_seat_count THEN
                ROLLBACK;
                SELECT 'CANCEL_CONFLICT' AS Outcome;
            ELSE
                UPDATE Reservations
                SET StatusId = 4,
                    CancelledAtUtc = CURRENT_TIMESTAMP(6)
                WHERE Id = v_reservation_id;

                COMMIT;

                SELECT
                    'CANCELLED' AS Outcome,
                    r.ReservationGuid, s.ShowGuid, u.UserGuid,
                    COUNT(rs.SeatId) * s.PricePaise AS AmountPaise,
                    st.Name AS Status, se.SeatNumber
                FROM Reservations r
                JOIN Users u ON u.Id = r.UserId
                JOIN Shows s ON s.Id = r.ShowId
                JOIN EnumStatus st ON st.Id = r.StatusId
                JOIN ReservationSeats rs ON rs.ReservationId = r.Id
                JOIN Seats se ON se.Id = rs.SeatId
                WHERE r.Id = v_reservation_id
                GROUP BY r.Id, r.ReservationGuid, s.ShowGuid, u.UserGuid, s.PricePaise, st.Name, se.Id, se.SeatNumber
                ORDER BY se.Id;
            END IF;
        END IF;
    END IF;
END$$

CREATE PROCEDURE sp_get_available_seats_metrics()
BEGIN
    SELECT
        s.ShowGuid,
        SUM(CASE WHEN se.StatusId = 1 THEN 1 ELSE 0 END) AS AvailableSeats
    FROM Shows s
    LEFT JOIN Seats se ON se.ShowId = s.Id
    GROUP BY s.Id, s.ShowGuid
    ORDER BY s.Id;
END$$

DELIMITER ;

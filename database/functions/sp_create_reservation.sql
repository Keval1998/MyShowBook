DROP PROCEDURE IF EXISTS sp_create_reservation;

DELIMITER $$

-- Input temp table:
-- tmp_reservation_seats
-- (
--     SeatNumber VARCHAR(32) NOT NULL PRIMARY KEY
-- )

-- Internal locking result:
-- SeatId BIGINT, SeatNumber VARCHAR(32), StatusId INT
-- The API consumes this first result set only to complete the locking read.

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
        SELECT 8 AS Outcome;
    ELSE
        SELECT Id, PricePaise, PerUserLimit
        INTO v_show_id, v_price_paise, v_per_user_limit
        FROM Shows
        WHERE ShowGuid = p_show_guid
        LIMIT 1;

        IF v_show_id IS NULL THEN
            ROLLBACK;
            SELECT 7 AS Outcome;
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
                    SELECT 5 AS Outcome;
                ELSE
                    COMMIT;

                    SELECT
                        2 AS Outcome,
                        r.ReservationGuid,
                        s.ShowGuid,
                        u.UserGuid,
                        (
                            SELECT COUNT(*)
                            FROM ReservationSeats
                            WHERE ReservationId = r.Id
                        ) * s.PricePaise AS AmountPaise,
                        st.Name AS Status
                    FROM Reservations r
                    JOIN Users u ON u.Id = r.UserId
                    JOIN Shows s ON s.Id = r.ShowId
                    JOIN EnumStatus st ON st.Id = r.StatusId
                    WHERE r.Id = v_existing_id;

                    SELECT se.SeatNumber
                    FROM ReservationSeats rs
                    JOIN Seats se ON se.Id = rs.SeatId
                    WHERE rs.ReservationId = v_existing_id
                    ORDER BY se.SeatNumber;
                END IF;
            ELSE
                SELECT COUNT(*) INTO v_requested_count
                FROM tmp_reservation_seats;

                SELECT se.Id, se.SeatNumber, se.StatusId
                FROM Seats se
                JOIN tmp_reservation_seats requested
                  ON requested.SeatNumber = se.SeatNumber
                WHERE se.ShowId = v_show_id
                ORDER BY se.SeatNumber
                FOR UPDATE;

                SELECT COUNT(*)
                INTO v_found_count
                FROM Seats se
                JOIN tmp_reservation_seats requested
                  ON requested.SeatNumber = se.SeatNumber
                WHERE se.ShowId = v_show_id;

                SELECT COUNT(*)
                INTO v_unavailable_count
                FROM Seats se
                JOIN tmp_reservation_seats requested
                  ON requested.SeatNumber = se.SeatNumber
                WHERE se.ShowId = v_show_id
                  AND se.StatusId <> 1;

                IF v_found_count <> v_requested_count THEN
                    ROLLBACK;
                    SELECT 6 AS Outcome;
                ELSEIF v_unavailable_count > 0 THEN
                    ROLLBACK;
                    SELECT 3 AS Outcome;
                ELSE
                    SELECT COUNT(*)
                    INTO v_existing_count
                    FROM ReservationSeats rs
                    JOIN Reservations r ON r.Id = rs.ReservationId
                    WHERE r.UserId = v_user_id
                      AND r.ShowId = v_show_id
                      AND r.StatusId = 3;

                    IF v_existing_count + v_requested_count > v_per_user_limit THEN
                        ROLLBACK;
                        SELECT 4 AS Outcome;
                    ELSE
                        INSERT INTO Reservations (
                            ReservationGuid,
                            UserId,
                            ShowId,
                            StatusId,
                            IdempotencyKey,
                            RequestHash
                        )
                        VALUES (
                            p_reservation_guid,
                            v_user_id,
                            v_show_id,
                            3,
                            p_idempotency_key,
                            p_request_hash
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
                        ORDER BY se.SeatNumber;

                        COMMIT;

                        SELECT
                            1 AS Outcome,
                            r.ReservationGuid,
                            s.ShowGuid,
                            u.UserGuid,
                            v_requested_count * s.PricePaise AS AmountPaise,
                            st.Name AS Status
                        FROM Reservations r
                        JOIN Users u ON u.Id = r.UserId
                        JOIN Shows s ON s.Id = r.ShowId
                        JOIN EnumStatus st ON st.Id = r.StatusId
                        WHERE r.Id = v_reservation_id;

                        SELECT se.SeatNumber
                        FROM ReservationSeats rs
                        JOIN Seats se ON se.Id = rs.SeatId
                        WHERE rs.ReservationId = v_reservation_id
                        ORDER BY se.SeatNumber;
                    END IF;
                END IF;
            END IF;
        END IF;
    END IF;
END$$

DELIMITER ;

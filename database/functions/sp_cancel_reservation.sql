DROP PROCEDURE IF EXISTS sp_cancel_reservation;

DELIMITER $$

-- No input temp table is required.
-- Internal locking result:
-- SeatId BIGINT, SeatNumber VARCHAR(32), StatusId INT
-- The API consumes this first result set only to complete the locking read.

CREATE PROCEDURE sp_cancel_reservation(
    IN p_user_guid CHAR(36),
    IN p_reservation_guid CHAR(36)
)
BEGIN
    DECLARE v_user_id BIGINT DEFAULT NULL;
    DECLARE v_reservation_id BIGINT DEFAULT NULL;
    DECLARE v_status_id INT DEFAULT NULL;
    DECLARE v_seat_count INT DEFAULT 0;
    DECLARE v_confirmed_count INT DEFAULT 0;

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
        SELECT 2 AS Outcome;
    ELSE
        SELECT Id, StatusId
        INTO v_reservation_id, v_status_id
        FROM Reservations
        WHERE ReservationGuid = p_reservation_guid
          AND UserId = v_user_id
        FOR UPDATE;

        IF v_reservation_id IS NULL THEN
            ROLLBACK;
            SELECT 2 AS Outcome;
        ELSEIF v_status_id = 4 THEN
            COMMIT;

            SELECT
                1 AS Outcome,
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
            WHERE r.Id = v_reservation_id;

            SELECT se.SeatNumber
            FROM ReservationSeats rs
            JOIN Seats se ON se.Id = rs.SeatId
            WHERE rs.ReservationId = v_reservation_id
            ORDER BY se.SeatNumber;
        ELSE
            SELECT se.Id, se.SeatNumber, se.StatusId
            FROM ReservationSeats rs
            JOIN Seats se ON se.Id = rs.SeatId
            WHERE rs.ReservationId = v_reservation_id
            ORDER BY se.SeatNumber
            FOR UPDATE;

            SELECT COUNT(*)
            INTO v_seat_count
            FROM ReservationSeats
            WHERE ReservationId = v_reservation_id;

            SELECT COUNT(*)
            INTO v_confirmed_count
            FROM ReservationSeats rs
            JOIN Seats se ON se.Id = rs.SeatId
            WHERE rs.ReservationId = v_reservation_id
              AND se.StatusId = 3;

            IF v_confirmed_count <> v_seat_count THEN
                ROLLBACK;
                SELECT 3 AS Outcome;
            ELSE
                UPDATE Seats se
                JOIN ReservationSeats rs ON rs.SeatId = se.Id
                SET se.StatusId = 1
                WHERE rs.ReservationId = v_reservation_id;

                UPDATE Reservations
                SET StatusId = 4,
                    CancelledAtUtc = CURRENT_TIMESTAMP(6)
                WHERE Id = v_reservation_id;

                COMMIT;

                SELECT
                    1 AS Outcome,
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
                WHERE r.Id = v_reservation_id;

                SELECT se.SeatNumber
                FROM ReservationSeats rs
                JOIN Seats se ON se.Id = rs.SeatId
                WHERE rs.ReservationId = v_reservation_id
                ORDER BY se.SeatNumber;
            END IF;
        END IF;
    END IF;
END$$

DELIMITER ;

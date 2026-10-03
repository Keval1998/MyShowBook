DROP PROCEDURE IF EXISTS sp_create_show;

DELIMITER $$

-- Temp table required by this procedure:
-- tmp_show_seats
-- (
--     SeatNumber VARCHAR(32) NOT NULL PRIMARY KEY
-- )

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
        s.ShowGuid,
        s.Name,
        s.PricePaise,
        s.PerUserLimit,
        COUNT(se.Id) AS TotalSeats,
        SUM(CASE WHEN se.StatusId = 1 THEN 1 ELSE 0 END) AS AvailableSeats,
        SUM(CASE WHEN se.StatusId = 2 THEN 1 ELSE 0 END) AS HeldSeats,
        SUM(CASE WHEN se.StatusId = 3 THEN 1 ELSE 0 END) AS ConfirmedSeats
    FROM Shows s
    LEFT JOIN Seats se ON se.ShowId = s.Id
    WHERE s.Id = v_show_id
    GROUP BY s.Id;

    SELECT
        se.SeatGuid,
        se.SeatNumber,
        st.Name AS Status
    FROM Seats se
    JOIN EnumStatus st ON st.Id = se.StatusId
    WHERE se.ShowId = v_show_id
    ORDER BY se.SeatNumber;
END$$

DELIMITER ;

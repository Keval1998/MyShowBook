DROP PROCEDURE IF EXISTS sp_get_show;

DELIMITER $$

CREATE PROCEDURE sp_get_show(IN p_show_guid CHAR(36))
BEGIN
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
    WHERE s.ShowGuid = p_show_guid
    GROUP BY s.Id;

    SELECT
        se.SeatGuid,
        se.SeatNumber,
        st.Name AS Status
    FROM Seats se
    JOIN Shows s ON s.Id = se.ShowId
    JOIN EnumStatus st ON st.Id = se.StatusId
    WHERE s.ShowGuid = p_show_guid
    ORDER BY se.SeatNumber;
END$$

DELIMITER ;

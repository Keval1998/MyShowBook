DELIMITER //

CREATE PROCEDURE sp_list_shows()
BEGIN
    SELECT
        s.ShowGuid,
        s.Name,
        s.PricePaise,
        COUNT(se.Id) AS TotalSeats,
        COALESCE(SUM(se.StatusId = 1), 0) AS AvailableSeats,
        COALESCE(SUM(se.StatusId = 2), 0) AS HeldSeats,
        COALESCE(SUM(se.StatusId = 3), 0) AS ConfirmedSeats
    FROM Shows s
    LEFT JOIN Seats se ON se.ShowId = s.Id
    GROUP BY s.Id, s.ShowGuid, s.Name, s.PricePaise
    ORDER BY s.CreatedAtUtc DESC;
END //

DELIMITER ;
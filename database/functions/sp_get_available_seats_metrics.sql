DROP PROCEDURE IF EXISTS sp_get_available_seats_metrics;

DELIMITER $$

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

DROP PROCEDURE IF EXISTS sp_get_user_for_login;

DELIMITER $$

CREATE PROCEDURE sp_get_user_for_login(IN p_username VARCHAR(64))
BEGIN
    SELECT UserGuid, Username, PasswordHash, IsAdmin
    FROM Users
    WHERE Username = p_username
    LIMIT 1;
END$$

DELIMITER ;

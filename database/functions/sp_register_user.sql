DELIMITER //

CREATE PROCEDURE sp_register_user(
    IN p_user_guid CHAR(36),
    IN p_username VARCHAR(64),
    IN p_password_hash VARCHAR(255),
    IN p_is_admin BOOLEAN
)
BEGIN
    INSERT INTO Users (UserGuid, Username, PasswordHash, IsAdmin)
    VALUES (p_user_guid, p_username, p_password_hash, p_is_admin);

    SELECT UserGuid, Username, IsAdmin
    FROM Users
    WHERE UserGuid = p_user_guid;
END //

DELIMITER ;
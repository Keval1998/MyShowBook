CREATE TABLE EnumStatus (
    Id INT NOT NULL,
    Name VARCHAR(32) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY UX_EnumStatus_Name (Name)
) ENGINE=InnoDB;

CREATE TABLE Users (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    UserGuid CHAR(36) NOT NULL,
    Username VARCHAR(64) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    IsAdmin BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAtUtc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY UX_Users_UserGuid (UserGuid),
    UNIQUE KEY UX_Users_Username (Username)
) ENGINE=InnoDB;

CREATE TABLE Shows (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    ShowGuid CHAR(36) NOT NULL,
    Name VARCHAR(128) NOT NULL,
    PricePaise BIGINT NOT NULL,
    PerUserLimit INT NOT NULL DEFAULT 4,
    CreatedAtUtc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY UX_Shows_ShowGuid (ShowGuid),
    CONSTRAINT CK_Shows_PricePaise CHECK (PricePaise >= 0),
    CONSTRAINT CK_Shows_PerUserLimit CHECK (PerUserLimit > 0)
) ENGINE=InnoDB;

CREATE TABLE Seats (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    SeatGuid CHAR(36) NOT NULL,
    ShowId BIGINT NOT NULL,
    SeatNumber VARCHAR(32) NOT NULL,
    StatusId INT NOT NULL,
    CreatedAtUtc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY UX_Seats_SeatGuid (SeatGuid),
    UNIQUE KEY UX_Seats_Show_SeatNumber (ShowId, SeatNumber),
    KEY IX_Seats_Show_Status (ShowId, StatusId),
    CONSTRAINT FK_Seats_Show FOREIGN KEY (ShowId) REFERENCES Shows (Id),
    CONSTRAINT FK_Seats_Status FOREIGN KEY (StatusId) REFERENCES EnumStatus (Id)
) ENGINE=InnoDB;

CREATE TABLE Reservations (
    Id BIGINT NOT NULL AUTO_INCREMENT,
    ReservationGuid CHAR(36) NOT NULL,
    UserId BIGINT NOT NULL,
    ShowId BIGINT NOT NULL,
    StatusId INT NOT NULL,
    IdempotencyKey VARCHAR(128) NOT NULL,
    RequestHash CHAR(64) NOT NULL,
    CreatedAtUtc DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CancelledAtUtc DATETIME(6) NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY UX_Reservations_ReservationGuid (ReservationGuid),
    UNIQUE KEY UX_Reservations_User_Idempotency (UserId, IdempotencyKey),
    KEY IX_Reservations_Show_Status (ShowId, StatusId),
    KEY IX_Reservations_User_Status (UserId, StatusId),
    CONSTRAINT FK_Reservations_User FOREIGN KEY (UserId) REFERENCES Users (Id),
    CONSTRAINT FK_Reservations_Show FOREIGN KEY (ShowId) REFERENCES Shows (Id),
    CONSTRAINT FK_Reservations_Status FOREIGN KEY (StatusId) REFERENCES EnumStatus (Id)
) ENGINE=InnoDB;

CREATE TABLE ReservationSeats (
    ReservationId BIGINT NOT NULL,
    SeatId BIGINT NOT NULL,
    PRIMARY KEY (ReservationId, SeatId),
    KEY IX_ReservationSeats_Seat (SeatId),
    CONSTRAINT FK_ReservationSeats_Reservation FOREIGN KEY (ReservationId) REFERENCES Reservations (Id),
    CONSTRAINT FK_ReservationSeats_Seat FOREIGN KEY (SeatId) REFERENCES Seats (Id)
) ENGINE=InnoDB;

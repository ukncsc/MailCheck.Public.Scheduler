ALTER TABLE scheduled_reminders
    ADD last_fired DATETIME DEFAULT NULL,
    ADD times_fired bigint DEFAULT 0,
    ADD last_successful DATETIME DEFAULT NULL,
    ADD times_successful bigint DEFAULT 0;    
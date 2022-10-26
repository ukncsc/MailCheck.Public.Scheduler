CREATE TABLE `scheduled_reminders` (
  `service` varchar(255) NOT NULL,
  `resource_id` varchar(255) NOT NULL,
  `scheduled_time` datetime NOT NULL,
  PRIMARY KEY (`service`,`resource_id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
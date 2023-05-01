### Master Attendance Reward 
```
CREATE TABLE `master_attendance_reward` (
  `dayId` int NOT NULL,
  `itemId` int NOT NULL,
  `quantity` int NOT NULL,
  PRIMARY KEY (`dayId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```


### Player Attendance Log
```
CREATE TABLE `player_attendance_log` (
  `id` int NOT NULL AUTO_INCREMENT,
  `userId` int NOT NULL,
  `date` date NOT NULL DEFAULT (curdate()),
  `sequenceDayCount` int NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb3
```
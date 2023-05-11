### mailbox
```
CREATE TABLE `mailbox` (
  `mailId` int NOT NULL AUTO_INCREMENT,
  `senderId` int NOT NULL,
  `receiverId` int NOT NULL,
  `title` varchar(45) NOT NULL,
  `content` varchar(1024) NOT NULL,
  `sendDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `isRead` int NOT NULL,
  `hasItem` int NOT NULL,
  `isDeleted` int NOT NULL,
  PRIMARY KEY (`mailId`)
) ENGINE=InnoDB AUTO_INCREMENT=151 DEFAULT CHARSET=utf8mb3
```






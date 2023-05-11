### mailbox
```
CREATE TABLE `mailbox` (
  `mailId` int NOT NULL AUTO_INCREMENT,
  `senderId` int NOT NULL,
  `receiverId` int NOT NULL,
  `title` varchar(45) NOT NULL,
  `content` varchar(1024) NOT NULL,
  `sendDate` date NOT NULL DEFAULT (curdate()),
  `isRead` int NOT NULL,
  `isDeleted` int NOT NULL,
  `itemId` int NOT NULL,
  `quantity` int NOT NULL,
  `expireDate` date DEFAULT NULL,
  `hasReceived` int NOT NULL,
  PRIMARY KEY (`mailId`)
) ENGINE=InnoDB AUTO_INCREMENT=191 DEFAULT CHARSET=utf8mb3
```






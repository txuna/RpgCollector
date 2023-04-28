### mailbox
```
CREATE TABLE `mailbox` (
  `mailId` int NOT NULL,
  `senderId` int NOT NULL,
  `receiverId` int NOT NULL,
  `title` varchar(45) NOT NULL,
  `content` varchar(1024) NOT NULL,
  `sendDate` date NOT NULL,
  `isRead` int NOT NULL,
  `hasItem` int NOT NULL,
  PRIMARY KEY (`mailId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```

### mail_item
```
CREATE TABLE `mail_item` (
  `mailId` int NOT NULL,
  `itemId` int NOT NULL,
  `quantity` int NOT NULL,
  `hasReceived` int NOT NULL,
  PRIMARY KEY (`mailId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```










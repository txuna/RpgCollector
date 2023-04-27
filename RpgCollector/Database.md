# Database Table 


## Account Database 
### users
```
CREATE TABLE `users` (
  `userId` int NOT NULL AUTO_INCREMENT,
  `userName` varchar(45) NOT NULL,
  `password` varchar(256) NOT NULL,
  `passwordSalt` varchar(256) NOT NULL,
  `permission` int NOT NULL,
  PRIMARY KEY (`userId`,`userName`),
  UNIQUE KEY `user_id_UNIQUE` (`userName`),
  UNIQUE KEY `index_UNIQUE` (`userId`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb3
```



## Game Database
### players
```
CREATE TABLE `players` (
  `userId` int NOT NULL,
  `currentHealth` int NOT NULL,
  `maxHealth` int NOT NULL,
  `currentExp` int NOT NULL,
  `maxExp` int NOT NULL,
  `level` int NOT NULL,
  `money` int NOT NULL,
  PRIMARY KEY (`userId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```

### notices 
```
CREATE TABLE `notices` (
  `noticeId` int NOT NULL AUTO_INCREMENT,
  `content` varchar(1024) NOT NULL,
  `uploaderId` int NOT NULL,
  PRIMARY KEY (`noticeId`),
  UNIQUE KEY `noticeId_UNIQUE` (`noticeId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```

### mailbox
```
CREATE TABLE `tuuna_game`.`mailbox` (
  `mailId` INT NOT NULL,
  `senderId` INT NOT NULL,
  `receiverId` INT NOT NULL,
  `title` VARCHAR(45) NOT NULL,
  `content` VARCHAR(1024) NOT NULL,
  `sendDate` DATE NOT NULL,
  `isRead` INT NOT NULL,
  `hasItem` INT NOT NULL,
  PRIMARY KEY (`mailId`));
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

### items
```
CREATE TABLE `items` (
  `itemId` int NOT NULL,
  `itemName` varchar(45) NOT NULL,
  `attributeId` int NOT NULL,
  `enchantMaxCount` int NOT NULL,
  PRIMARY KEY (`itemId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```

### player_inventory 
```
CREATE TABLE `player_inventory` (
  `playerId` int NOT NULL,
  `itemId` int DEFAULT NULL,
  `quantity` int DEFAULT NULL,
  PRIMARY KEY (`playerId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```

### item_price 
```
CREATE TABLE `item_price` (
  `itemId` int NOT NULL,
  `sellPrice` int NOT NULL,
  `buyPrice` int NOT NULL,
  PRIMARY KEY (`itemId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```

### attribute_info 
```
CREATE TABLE `attribute_info` (
  `attributeId` int NOT NULL,
  `typeId` int NOT NULL,
  PRIMARY KEY (`attributeId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```


### type_ifno
```
CREATE TABLE `type_info` (
  `typeId` int NOT NULL,
  `typeName` varchar(45) NOT NULL,
  PRIMARY KEY (`typeId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```
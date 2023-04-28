### players
```
CREATE TABLE `players` (
  `userId` int NOT NULL,
  `hp` int NOT NULL,
  `exp` int NOT NULL,
  `level` int NOT NULL,
  `money` int NOT NULL,
  PRIMARY KEY (`userId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```

### player_items 
```
CREATE TABLE `player_items` (
  `playerItemId` int NOT NULL AUTO_INCREMENT,
  `userId` int NOT NULL,
  `itemId` int NOT NULL,
  `quantity` int NOT NULL,
  `enchantCount` int NOT NULL,
  PRIMARY KEY (`playerItemId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```


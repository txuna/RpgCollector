### player_enchant_log 
```
CREATE TABLE `player_enchant_log` (
  `id` int NOT NULL,
  `playerItemId` int NOT NULL,
  `userId` int NOT NULL,
  `enchantCount` int NOT NULL,
  `date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```


### master_enchant_info
```
CREATE TABLE `master_enchant_info` (
  `enchantCount` int NOT NULL,
  `percent` int NOT NULL,
  `increasementValue` int NOT NULL,
  `price` int NOT NULL,
  PRIMARY KEY (`enchantCount`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```
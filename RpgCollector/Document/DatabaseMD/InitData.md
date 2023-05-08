### Player Init State 
```
CREATE TABLE `init_player_state` (
  `id` int NOT NULL,
  `hp` int NOT NULL,
  `exp` int NOT NULL,
  `money` int NOT NULL,
  `level` int NOT NULL,
  `attack` int NOT NULL,
  `defence` int NOT NULL,
  `magic` int NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```

### Player Init Items 
```
CREATE TABLE `init_player_items` (
  `itemId` int NOT NULL,
  `quantity` int NOT NULL,
  PRIMARY KEY (`itemId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```
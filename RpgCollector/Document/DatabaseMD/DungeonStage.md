## Dungeon Stage Database 
### Player Stage Clear Info 
```
CREATE TABLE `player_stage_info` (
  `userId` int NOT NULL,
  `stageId` int NOT NULL,
  PRIMARY KEY (`userId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```

### Master Stage Info 
```
CREATE TABLE `master_stage_info` (
  `stageId` int NOT NULL,
  `stageName` varchar(45) NOT NULL,
  PRIMARY KEY (`stageId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```

### Master Stage Item 
```
CREATE TABLE `master_stage_farming_item` (
  `id` int NOT NULL AUTO_INCREMENT,
  `stageId` int NOT NULL,
  `itemId` int NOT NULL,
  `quantity` int NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```

### Master Stage Npc 
```
CREATE TABLE `master_stage_npc` (
  `id` int NOT NULL AUTO_INCREMENT,
  `stageId` int NOT NULL,
  `npcId` int NOT NULL,
  `count` int NOT NULL,
  `exp` int NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```
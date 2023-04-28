### master_item_info 
```
CREATE TABLE `master_item_info` (
  `itemId` int NOT NULL,
  `itemName` varchar(45) NOT NULL,
  `attributeId` int NOT NULL,
  `sellPrice` int NOT NULL,
  `buyPrice` int NOT NULL,
  `canLevel` int NOT NULL,
  `attack` int NOT NULL,
  `defence` int NOT NULL,
  `magic` int NOT NULL,
  `maxEnchantCount` int NOT NULL,
  PRIMARY KEY (`itemId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```


### mater_item_attribute 
```
CREATE TABLE `master_item_attribute` (
  `attributeId` int NOT NULL,
  `typeId` int NOT NULL,
  `attributeName` varchar(45) NOT NULL,
  PRIMARY KEY (`attributeId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```


### master_item_type
```
CREATE TABLE `master_item_type` (
  `typeId` int NOT NULL,
  `typeName` varchar(45) NOT NULL,
  PRIMARY KEY (`typeId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```
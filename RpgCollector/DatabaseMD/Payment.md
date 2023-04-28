### Master Package Item
```
CREATE TABLE `master_package_info` (
  `packageId` int NOT NULL,
  `itemId` int NOT NULL,
  `quantity` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```

### Master Package Price 
```
CREATE TABLE `master_package_price` (
  `packageId` int NOT NULL,
  `price` int NOT NULL,
  PRIMARY KEY (`packageId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```

### Payment Table 
```
CREATE TABLE `player_payment_info` (
  `receiptId` int NOT NULL,
  `userId` int NOT NULL,
  `packageId` varchar(45) NOT NULL,
  PRIMARY KEY (`receiptId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
```
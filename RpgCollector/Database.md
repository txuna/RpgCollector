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


### items 
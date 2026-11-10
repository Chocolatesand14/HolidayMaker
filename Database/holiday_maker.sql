-- MySQL dump 10.13  Distrib 8.4.13, for Linux (x86_64)
--
-- Host: localhost    Database: holiday_maker
-- ------------------------------------------------------
-- Server version	8.4.13

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `holiday_maker`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `holiday_maker` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `holiday_maker`;

--
-- Table structure for table `accommodation`
--

DROP TABLE IF EXISTS `accommodation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `accommodation` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(150) NOT NULL,
  `city` varchar(100) NOT NULL,
  `type` varchar(30) NOT NULL,
  `distance_to_beach_m` int NOT NULL,
  `distance_to_center_m` int NOT NULL,
  `rating` tinyint NOT NULL,
  PRIMARY KEY (`id`),
  CONSTRAINT `accommodation_chk_1` CHECK ((`distance_to_beach_m` >= 0)),
  CONSTRAINT `accommodation_chk_2` CHECK ((`distance_to_center_m` >= 0)),
  CONSTRAINT `accommodation_chk_3` CHECK ((`rating` between 1 and 5))
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `accommodation`
--

LOCK TABLES `accommodation` WRITE;
/*!40000 ALTER TABLE `accommodation` DISABLE KEYS */;
INSERT INTO `accommodation` VALUES (1,'Tomtens Sista Utväg','Malmö','Hotel',400,1200,5),(2,'Saltstänk & Solsken','Helsingborg','Hotel',150,2000,4),(3,'Grand Bellevue','Malmö','Hotel',3200,150,5),(4,'Havsbris Pensionat','Helsingborg','Pensionat',90,2400,3),(5,'The Sleepy Fox','Malmö','Hotel',2100,700,4);
/*!40000 ALTER TABLE `accommodation` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `booking`
--

DROP TABLE IF EXISTS `booking`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `booking` (
  `id` int NOT NULL AUTO_INCREMENT,
  `customer_id` int NOT NULL,
  `room_id` int NOT NULL,
  `start_date` date NOT NULL,
  `end_date` date NOT NULL,
  `number_of_guests` int NOT NULL,
  PRIMARY KEY (`id`),
  KEY `customer_id` (`customer_id`),
  KEY `room_id` (`room_id`),
  CONSTRAINT `booking_ibfk_1` FOREIGN KEY (`customer_id`) REFERENCES `customer` (`id`),
  CONSTRAINT `booking_ibfk_2` FOREIGN KEY (`room_id`) REFERENCES `room` (`id`),
  CONSTRAINT `booking_chk_1` CHECK ((`number_of_guests` > 0)),
  CONSTRAINT `booking_chk_2` CHECK ((`end_date` > `start_date`))
) ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `booking`
--

LOCK TABLES `booking` WRITE;
/*!40000 ALTER TABLE `booking` DISABLE KEYS */;
INSERT INTO `booking` VALUES (1,1,1,'2026-12-10','2026-12-13',1),(2,2,1,'2026-12-13','2026-12-16',1),(3,1,2,'2026-12-01','2026-12-04',1),(4,2,3,'2026-12-05','2026-12-09',2),(5,3,9,'2026-12-06','2026-12-08',1),(6,4,14,'2026-12-11','2026-12-15',2),(7,5,20,'2026-12-18','2026-12-22',4),(8,1,4,'2026-12-01','2026-12-03',2),(9,2,12,'2026-12-12','2026-12-15',4),(10,3,13,'2026-12-08','2026-12-11',1),(11,4,18,'2026-12-16','2026-12-19',2),(12,5,24,'2026-12-23','2026-12-27',4),(13,6,7,'2026-12-04','2026-12-08',4),(14,7,8,'2026-12-18','2026-12-22',3),(15,6,16,'2026-12-20','2026-12-24',4),(16,1,19,'2026-12-10','2026-12-12',2),(17,7,23,'2026-12-06','2026-12-09',2),(18,1,1,'2026-12-16','2026-12-19',1),(19,2,2,'2026-12-04','2026-12-06',1),(20,3,10,'2026-12-02','2026-12-07',2),(21,4,11,'2026-12-14','2026-12-16',2),(22,5,15,'2026-12-21','2026-12-25',2),(23,1,17,'2026-12-01','2026-12-02',1),(24,2,21,'2026-12-07','2026-12-11',1),(25,3,22,'2026-12-24','2026-12-28',2);
/*!40000 ALTER TABLE `booking` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `customer`
--

DROP TABLE IF EXISTS `customer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `customer` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `email` varchar(255) NOT NULL,
  `phone_number` varchar(30) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `email` (`email`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `customer`
--

LOCK TABLES `customer` WRITE;
/*!40000 ALTER TABLE `customer` DISABLE KEYS */;
INSERT INTO `customer` VALUES (1,'Draven Blackwood','draven.blackwood@example.com','0701234567'),(2,'Gustav Vasa-Lopp','gustav.vasalopp@example.com','0702345678'),(3,'Britt-Marie Kommitté','brittmarie.kommitte@example.com','0703456789'),(4,'Olof Neutralsson','olof.neutralsson@example.com','0704567890'),(5,'Kent Skattefogde','kent.skattefogde@example.com','0705678901'),(6,'Gunnar Kaosson','gunnar.kaosson@example.com','0700000011'),(7,'Lotta Fullt-Upp','lotta.fulltupp@example.com','0700000012'),(8,'Börje Semesterstress','borje.semesterstress@example.com','0700000013');
/*!40000 ALTER TABLE `customer` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `facility`
--

DROP TABLE IF EXISTS `facility`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `facility` (
  `id` int NOT NULL AUTO_INCREMENT,
  `accommodation_id` int NOT NULL,
  `has_pool` tinyint(1) NOT NULL DEFAULT '0',
  `has_restaurant` tinyint(1) NOT NULL DEFAULT '0',
  `has_wifi` tinyint(1) NOT NULL DEFAULT '0',
  `has_parking` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `accommodation_id` (`accommodation_id`),
  CONSTRAINT `facility_ibfk_1` FOREIGN KEY (`accommodation_id`) REFERENCES `accommodation` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `facility`
--

LOCK TABLES `facility` WRITE;
/*!40000 ALTER TABLE `facility` DISABLE KEYS */;
INSERT INTO `facility` VALUES (1,1,1,1,1,1),(2,2,1,0,1,1),(3,3,1,1,1,0),(4,4,0,1,1,0),(5,5,0,0,1,1);
/*!40000 ALTER TABLE `facility` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `payment`
--

DROP TABLE IF EXISTS `payment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payment` (
  `id` int NOT NULL AUTO_INCREMENT,
  `booking_id` int NOT NULL,
  `amount` decimal(10,2) NOT NULL,
  `payment_date` date NOT NULL,
  PRIMARY KEY (`id`),
  KEY `booking_id` (`booking_id`),
  CONSTRAINT `payment_ibfk_1` FOREIGN KEY (`booking_id`) REFERENCES `booking` (`id`),
  CONSTRAINT `payment_chk_1` CHECK ((`amount` > 0))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `payment`
--

LOCK TABLES `payment` WRITE;
/*!40000 ALTER TABLE `payment` DISABLE KEYS */;
/*!40000 ALTER TABLE `payment` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `room`
--

DROP TABLE IF EXISTS `room`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `room` (
  `id` int NOT NULL AUTO_INCREMENT,
  `accommodation_id` int NOT NULL,
  `room_number` varchar(20) NOT NULL,
  `max_guests` int NOT NULL,
  `price_per_night` decimal(10,2) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `accommodation_id` (`accommodation_id`,`room_number`),
  CONSTRAINT `room_ibfk_1` FOREIGN KEY (`accommodation_id`) REFERENCES `accommodation` (`id`),
  CONSTRAINT `room_chk_1` CHECK ((`max_guests` > 0)),
  CONSTRAINT `room_chk_2` CHECK ((`price_per_night` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=25 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `room`
--

LOCK TABLES `room` WRITE;
/*!40000 ALTER TABLE `room` DISABLE KEYS */;
INSERT INTO `room` VALUES (1,1,'101',1,790.00),(2,1,'102',1,850.00),(3,1,'103',2,1190.00),(4,1,'104',2,1250.00),(5,1,'105',2,1290.00),(6,1,'106',2,1390.00),(7,1,'107',4,1690.00),(8,1,'108',4,1890.00),(9,2,'201',1,690.00),(10,2,'202',2,990.00),(11,2,'203',2,1090.00),(12,2,'204',4,1490.00),(13,3,'301',1,990.00),(14,3,'302',2,1490.00),(15,3,'303',2,1690.00),(16,3,'304',4,2490.00),(17,4,'401',1,590.00),(18,4,'402',2,790.00),(19,4,'403',2,890.00),(20,4,'404',4,1290.00),(21,5,'501',1,650.00),(22,5,'502',2,950.00),(23,5,'503',2,1150.00),(24,5,'504',4,1590.00);
/*!40000 ALTER TABLE `room` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-11-10 13:43:06

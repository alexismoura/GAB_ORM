-- Script Date: 23/01/2015 11:28  - ErikEJ.SqlCeScripting version 3.5.2.40
-- Database information:
-- Database: D:\PROJETO\C_Sharp\GAB_ORM\GAB.Test\DB.lite
-- ServerVersion: 3.8.5
-- DatabaseSize: 2 KB
-- Created: 23/01/2015 11:25

-- User Table information:
-- Number of tables: 1
-- AreaProfissional: -1 row(s)

SELECT 1;
PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE AreaProfissional (
  AreaProfissionalCodigo integer  primary key auto increment
, AreaProfissionalSegmento integer  NOT NULL
, AreaProfissionalDescricao varchar(200) NOT NULL
);
COMMIT;


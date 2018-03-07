<?php 
 try {
            $dbhost = 'liampa4.cicnj8ihihlm.us-east-2.rds.amazonaws.com';
            $dbport = '3306';
            $dbname = 'nbapa4';
            $charset = 'utf8' ;
            $username = 'liambrozik';
            $password = '1234QWERa';
            $pdo = new PDO('mysql:host=liampa4.cicnj8ihihlm.us-east-2.rds.amazonaws.com;dbname=nbapa4','liambrozik','1234QWERa');
 } catch(PDOException $e) {
            echo 'ERROR: ' . $e->getMessage();
 }
?>
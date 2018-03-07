
 <?php 

header('Content-type: application/json');

        error_reporting(0);

        
        $Name = '%'.$_GET['search'].'%';
        //$Name = '%Lebron%';

        include 'database.php';

        $stmt = $pdo->prepare('SELECT * FROM TBL_NAME WHERE Name LIKE :Name');
        $stmt->execute(array('Name' => $Name));
        
        $result = $stmt->fetchAll();

        if (count($result) ) { 

            foreach($result as $row) {
            $row = new Player($row);

                $response["Name"] = $row->Name;
                $response["Team"] = $row->Team;
                $response["GP"] = $row->GP;
                $response["PPG"] = $row->PPG;
                $response["THREEPTM"] = $row->THREEPTM;
                $response["REB"] = $row->REB;
                $response["AST"] = $row->AST;
                
                //echo json_encode($response);
                echo $_GET['callback'] . '(' .  json_encode($response) . ')';
            }   
        } else {
            echo "No players found.";
        }
        class Player 
        {
            public $Name;
            public $Team;
            public $GP;
            public $PPG; 
            public $THREEPTM;
            public $REB; 
            public $AST;

    
            public function __construct($PlayerData)
            {
                $this->Name = substr($PlayerData['Name'], 1);
                $this->Team = $PlayerData['COL 2'];
                $this->GP = substr($PlayerData['COL 3'], 0, -1);
                $this->PPG = substr($PlayerData['COL 22'], 0, -1);
                $this->THREEPTM = substr($PlayerData['COL 8'], 0, -1);
                $this->REB = substr($PlayerData['COL 16'], 0, -1);
                $this->AST = substr($PlayerData['COL 7'], 0, -1);
            }
    

        }
        
    ?>
    
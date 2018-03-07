<?php
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
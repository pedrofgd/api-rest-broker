package main

import (
	"database/sql"
	"encoding/json"
	"fmt"
	"strings"
	"time"
	
	_ "github.com/mattn/go-sqlite3"
)

type Object struct {
	field_1 string `json:"field_1"`
	field_2 string `json:"field_2"`
	field_3 string `json:"field_3"`
	field_4 string `json:"field_4"`
	field_5 string `json:"field_5"`
	field_6 string `json:"field_6"`
	field_7 string `json:"field_7"`
	field_8 string `json:"field_8"`
	field_9 string `json:"field_9"`
	field_10 string `json:"field_10"`
	field_11 string `json:"field_11"`
	field_12 string `json:"field_12"`
	field_13 string `json:"field_13"`
	field_14 string `json:"field_14"`
	field_15 string `json:"field_15"`
}

func main() {
	start := time.Now()

	var rname = "randomtest"

	obj := map[string]interface{}{
		"field_1": "INFORMATION 1",
		"field_2": "INFORMATION 2",
		"field_3": "INFORMATION 3",
		"field_4": "INFORMATION 4",
		"field_5": "INFORMATION 5",
		"field_6": "INFORMATION 6",
		"field_7": "INFORMATION 7",
		"field_8": "INFORMATION 8",
		"field_9": "INFORMATION 9",
		"field_10": "INFORMATION 10",
		"field_11": "INFORMATION 11",
		"field_12": "INFORMATION 12",
		"field_13": "INFORMATION 13",
		"field_14": "INFORMATION 14",
		"field_15": "INFORMATION 15",
	}

	jsonData, _ := json.MarshalIndent(obj, "", " ")
	
	db, err := sql.Open("sqlite3", "./proxy.db")
	checkErr(err)
	
	query := fmt.Sprintf("select p.id from providers p join resources r on p.resource_id = r.id where name = '%s';", rname)
	rows, err := db.Query(query) 
	checkErr(err)
	var pid int
	
	for rows.Next() {
		err = rows.Scan(&pid)
		checkErr(err)
	}
	
	dquery := fmt.Sprintf("select * from dictionary d where provider_id = %d and payload_type = 'request';", pid)
	dictionary, err := db.Query(dquery)
	checkErr(err)
	var id int
	var provider_id string
	var payload_type string
	var response_http_status_code int
	var provider_key_name string
	var resource_key_name string
	
	mappedJson := string(jsonData)
	for dictionary.Next() {
		err = dictionary.Scan(&id, &provider_id, &payload_type, &response_http_status_code, &provider_key_name, &resource_key_name)
		checkErr(err)
		mappedJson = strings.Replace(mappedJson, resource_key_name, provider_key_name, 1)
		// fmt.Println(resource_key_name)
		// fmt.Println(provider_key_name)
	}
	
	db.Close()

	fmt.Printf("json data: %s\n", mappedJson)

	elapsed := time.Since(start)
	fmt.Printf("Binomial took %s (microseconds)\n", elapsed)
}

func checkErr(err error) {
	if err != nil {
		panic(err)
	}
}
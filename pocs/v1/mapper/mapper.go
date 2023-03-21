package main

import (
	"database/sql"
	"encoding/json"
	"fmt"
	"strings"
	"time"

	_ "github.com/mattn/go-sqlite3"
)

func Mappper(rname string, obj map[string]interface{}) string {
	start := time.Now()

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
	db.Close()

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
		rplcFrom := fmt.Sprintf("%q", resource_key_name)
		rplcTo := fmt.Sprintf("%q", provider_key_name)
		mappedJson = strings.Replace(mappedJson, rplcFrom, rplcTo, 1)
	}

	elapsed := time.Since(start)
	fmt.Printf("Map took %s (microseconds)\n", elapsed)

	return mappedJson
}

func checkErr(err error) {
	if err != nil {
		panic(err)
	}
}

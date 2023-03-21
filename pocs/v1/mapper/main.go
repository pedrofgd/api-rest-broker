package main

import (
	"encoding/json"
	"net/http"

	"github.com/gin-gonic/gin"
)

func main() {
	router := gin.Default()
	router.POST("/:resource", handleRequest)

	router.Run("localhost:8080")
}

func handleRequest(c *gin.Context) {
	var body map[string]interface{}
	if err := c.BindJSON(&body); err != nil {
		return
	}
	
	resource := c.Param("resource")
	mapped := Mappper(resource, body)
	var jsonMapped any
	json.Unmarshal([]byte(mapped), &jsonMapped)

	c.IndentedJSON(http.StatusOK, body)
}

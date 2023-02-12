package main

import (
	"compress/gzip"
	"fmt"
	"io"
	"log"
	"net/http"
	"net/url"
	"time"
)

func doRequest(req *http.Request) (*http.Response, error) {
	start := time.Now()
	res, err := http.DefaultClient.Do(req)
	elapsed := time.Since(start)
	log.Printf("Time elapsed for request: %s\n", elapsed)
	return res, err
}

func main() {
	// define origin server URL
	originServerURL, err := url.Parse("https://viacep.com.br")
	if err != nil {
		log.Fatal("invalid origin server URL")
	}

	reverseProxy := http.HandlerFunc(func(rw http.ResponseWriter, req *http.Request) {
		start := time.Now()
		fmt.Printf("[reverse proxy server] received request at: %s\n", time.Now())

		// set req Host, URL and Request URI to forward a request to the origin server
		req.Host = originServerURL.Host
		req.URL.Host = originServerURL.Host
		req.URL.Scheme = originServerURL.Scheme
		req.RequestURI = ""

		// save the response from the origin server
		originServerResponse, err := doRequest(req)
		if err != nil {
			rw.WriteHeader(http.StatusInternalServerError)
			_, _ = fmt.Fprint(rw, err)
			return
		}

		// return response to the client
		rw.Header().Set("Content-Type", "application/json; charset=utf-8")
		rw.WriteHeader(http.StatusOK)
		if originServerResponse.Header.Get("Content-Encoding") == "gzip" {
			reader, err := gzip.NewReader(originServerResponse.Body)
			if err != nil {
				rw.WriteHeader(http.StatusInternalServerError)
				_, _ = fmt.Fprint(rw, err)
				return
			}
			defer reader.Close()
			_, _ = io.Copy(rw, reader)
		} else {
			_, _ = io.Copy(rw, originServerResponse.Body)
		}

		io.Copy(rw, originServerResponse.Body)

		elapsed := time.Since(start)
		log.Printf("Total time elapsed for proxy: %s\n", elapsed)
	})

	log.Fatal(http.ListenAndServe(":8080", reverseProxy))
}

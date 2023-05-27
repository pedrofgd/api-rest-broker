package com.boker.fakeprovider;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;

import java.util.Random;

@SpringBootApplication
public class FakeProviderApplication {

    public static void main(String[] args) {

        SpringApplication.run(FakeProviderApplication.class, args);
    }

    @Bean
    public Random getRandom(){
        return new Random();
    }

}

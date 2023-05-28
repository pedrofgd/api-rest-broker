package com.boker.fakeprovider;

import lombok.AllArgsConstructor;
import lombok.extern.log4j.Log4j2;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.context.request.async.DeferredResult;

import java.time.LocalDateTime;
import java.util.Random;
import java.util.UUID;
import java.util.concurrent.ForkJoinPool;

@Log4j2
@RestController
@AllArgsConstructor
@RequestMapping("/")
public class TestContollerSeq {

    @PostMapping("/correios-alt/{cep}")
    public DeferredResult<ResponseEntity<CorreiosAltDTO>> correiosAltTest(@PathVariable String cep) {
        log.info("INÍCIO REQUISIÇÃO PARA CORREIOS-ALT:: {}", cep);

        var result = new DeferredResult<ResponseEntity<CorreiosAltDTO>>();
        var temp = LocalDateTime.now().getSecond();
        if (temp < 20) {
            log.error("FALHA REQUISIÇÃO PARA CORREIOS-ALT:: {}", cep);
            result.setResult(new ResponseEntity<>(HttpStatus.INTERNAL_SERVER_ERROR));
            return result;
        }
        ForkJoinPool.commonPool().submit(() -> {
            var dto = CorreiosAltDTO.builder()
                    .mensagem(UUID.randomUUID().toString())
                    .total(1)
                    .erro(false)
                    .dados(CorreiosAltDTO.Dados.builder()
                            .uf(UUID.randomUUID().toString())
                            .localidade(UUID.randomUUID().toString())
                            .locNoSem(UUID.randomUUID().toString())
                            .locNu(UUID.randomUUID().toString())
                            .localidadeSubordinada(UUID.randomUUID().toString())
                            .logradouroDNEC(UUID.randomUUID().toString())
                            .logradouroTextoAdicional(UUID.randomUUID().toString())
                            .logradouroTexto(UUID.randomUUID().toString())
                            .bairro(UUID.randomUUID().toString())
                            .baiNu(UUID.randomUUID().toString())
                            .nomeUnidade(UUID.randomUUID().toString())
                            .cep(cep)
                            .tipoCep(UUID.randomUUID().toString())
                            .numeroLocalidade(UUID.randomUUID().toString())
                            .situacao(UUID.randomUUID().toString())
                            .faixasCaixaPostal(UUID.randomUUID().toString())
                            .faixasCep(UUID.randomUUID().toString())
                            .build())
                    .build();
            result.setResult(new ResponseEntity<>(dto, HttpStatus.OK));
        });
        log.info("FIM DA REQUISIÇÃO PARA CORREIOS-ALT:: {}", cep);
        return result;
    }

    @GetMapping("/via-cep/{cep}")
    public DeferredResult<ResponseEntity<ViaCepDTO>> viaCepTest(@PathVariable String cep) {
        log.info("INÍCIO REQUISIÇÃO PARA VIA-CEP:: {}", cep);

        var result = new DeferredResult<ResponseEntity<ViaCepDTO>>();

        var temp = LocalDateTime.now().getSecond();
        if (temp >= 20 && temp < 40) {
            log.error("FALHA REQUISIÇÃO PARA VIA-CEP:: {}", cep);
            result.setResult(new ResponseEntity<>(HttpStatus.INTERNAL_SERVER_ERROR));
            return result;
        }

        ForkJoinPool.commonPool().submit(() -> {
            var dto = ViaCepDTO.builder()
                    .cep(cep)
                    .logradouro(UUID.randomUUID().toString())
                    .complemento(UUID.randomUUID().toString())
                    .bairro(UUID.randomUUID().toString())
                    .localidade(UUID.randomUUID().toString())
                    .uf(UUID.randomUUID().toString())
                    .ibge(UUID.randomUUID().toString())
                    .gia(UUID.randomUUID().toString())
                    .ddd(UUID.randomUUID().toString())
                    .siafi(UUID.randomUUID().toString())
                    .build();
            result.setResult(new ResponseEntity<>(dto, HttpStatus.OK));
        });

        log.info("FIM DA REQUISIÇÃO PARA VIA-CEP:: {}", cep);
        return result;
    }

    @GetMapping("/widenet/{cep}")
    public DeferredResult<ResponseEntity<WidenetDTO>> widenetTest(@PathVariable String cep) {
        log.info("INÍCIO REQUISIÇÃO PARA WIDENET:: {}", cep);

        var result = new DeferredResult<ResponseEntity<WidenetDTO>>();

        var temp = LocalDateTime.now().getSecond();
        if (temp >= 40) {
            log.error("FALHA REQUISIÇÃO PARA WIDENET:: {}", cep);
            result.setResult(new ResponseEntity<>(HttpStatus.INTERNAL_SERVER_ERROR));
            return result;
        }

        ForkJoinPool.commonPool().submit(() -> {
            var dto = WidenetDTO.builder()
                    .code(cep)
                    .state(UUID.randomUUID().toString())
                    .city(UUID.randomUUID().toString())
                    .district(UUID.randomUUID().toString())
                    .address(UUID.randomUUID().toString())
                    .build();
            result.setResult(new ResponseEntity<>(dto, HttpStatus.OK));
        });
        log.info("FIM DA REQUISIÇÃO PARA WIDENET:: {}", cep);
        return result;
    }

}

package com.boker.fakeprovider;

import lombok.AllArgsConstructor;
import lombok.extern.log4j.Log4j2;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.context.request.async.DeferredResult;

import java.util.Random;
import java.util.UUID;
import java.util.concurrent.ForkJoinPool;

@Log4j2
@RestController
@AllArgsConstructor
@RequestMapping("/")
public class TestContoller {
    private static final Integer availability = Integer.valueOf(System.getProperty("availability"));

    private Random random;

    @PostMapping("/correios-alt/{cep}")
    public ResponseEntity<DeferredResult<CorreiosAltDTO>> correiosAltTest(@PathVariable String cep) {
        log.info("INÍCIO REQUISIÇÃO PARA CORREIOS-ALT:: {}", cep);
        if (random.nextInt(1, 100) > availability) {
            log.error("FALHA REQUISIÇÃO PARA CORREIOS-ALT:: {}", cep);
            return new ResponseEntity<>(HttpStatus.INTERNAL_SERVER_ERROR);
        }
        var result = new DeferredResult<CorreiosAltDTO>();
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
            result.setResult(dto);
        });
        log.info("FIM DA REQUISIÇÃO PARA CORREIOS-ALT:: {}", cep);
        return new ResponseEntity<>(result, HttpStatus.OK);
    }

    @GetMapping("/via-cep/{cep}")
    public ResponseEntity<DeferredResult<ViaCepDTO>> viaCepTest(@PathVariable String cep) {
        log.info("INÍCIO REQUISIÇÃO PARA VIA-CEP:: {}", cep);
        if (random.nextInt(1, 100) > availability) {
            log.error("FALHA REQUISIÇÃO PARA VIA-CEP:: {}", cep);
            return new ResponseEntity<>(HttpStatus.INTERNAL_SERVER_ERROR);
        }
        var result = new DeferredResult<ViaCepDTO>();
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
            result.setResult(dto);
        });
        log.info("FIM DA REQUISIÇÃO PARA VIA-CEP:: {}", cep);
        return new ResponseEntity<>(result, HttpStatus.OK);
    }

    @GetMapping("/widenet/{cep}")
    public ResponseEntity<DeferredResult<WidenetDTO>> widenetTest(@PathVariable String cep) {
        log.info("INÍCIO REQUISIÇÃO PARA WIDENET:: {}", cep);
        if (random.nextInt(1, 100) > availability) {
            log.error("FALHA REQUISIÇÃO PARA WIDENET:: {}", cep);
            return new ResponseEntity<>(HttpStatus.INTERNAL_SERVER_ERROR);
        }
        var result = new DeferredResult<WidenetDTO>();
        ForkJoinPool.commonPool().submit(() -> {
            var dto = WidenetDTO.builder()
                    .code(cep)
                    .state(UUID.randomUUID().toString())
                    .city(UUID.randomUUID().toString())
                    .district(UUID.randomUUID().toString())
                    .address(UUID.randomUUID().toString())
                    .build();
            result.setResult(dto);
        });
        log.info("FIM DA REQUISIÇÃO PARA WIDENET:: {}", cep);
        return new ResponseEntity<>(result, HttpStatus.OK);
    }

}

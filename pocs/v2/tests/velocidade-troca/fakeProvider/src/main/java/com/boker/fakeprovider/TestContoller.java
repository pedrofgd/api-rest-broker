package com.boker.fakeprovider;

import lombok.extern.log4j.Log4j2;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.context.request.async.DeferredResult;

import java.util.UUID;
import java.util.concurrent.ForkJoinPool;

@Log4j2
@RestController
@RequestMapping("/")
public class TestContoller {
    private static final Long REQUEST_CORREIOS_ALT = 95L;
    private static final Long REQUEST_VIA_CEP = 90L;
    private static final Long REQUEST_WIDENET = 85L;
    private Long requests = 0L;

    @PostMapping("/correios-alt/{cep}")
    public DeferredResult<CorreiosAltDTO> correiosAltTest(@PathVariable String cep) {
        log.info("INÍCIO REQUISIÇÃO PARA CORREIOS-ALT:: {}",cep);
        requests++;
        if (requests % 100 >= REQUEST_CORREIOS_ALT) throw new RuntimeException();
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
        log.info("FIM DA REQUISIÇÃO PARA CORREIOS-ALT:: {}",cep);
        return result;
    }

    @GetMapping("/via-cep/{cep}")
    public DeferredResult<ViaCepDTO> viaCepTest(@PathVariable String cep) {
        log.info("INÍCIO REQUISIÇÃO PARA VIA-CEP:: {}",cep);
        requests++;
        if (requests % 100 >= REQUEST_VIA_CEP) throw new RuntimeException();
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
        log.info("FIM DA REQUISIÇÃO PARA VIA-CEP:: {}",cep);
        return result;
    }

    @GetMapping("/widenet/{cep}")
    public DeferredResult<WidenetDTO> widenetTest(@PathVariable String cep) {
        log.info("INÍCIO REQUISIÇÃO PARA WIDENET:: {}",cep);
        requests++;
        if (requests % 100 >= REQUEST_WIDENET) throw new RuntimeException();
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
        log.info("FIM DA REQUISIÇÃO PARA WIDENET:: {}",cep);
        return result;
    }

}

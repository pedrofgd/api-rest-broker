package com.boker.fakeprovider;

import lombok.*;

@AllArgsConstructor
@NoArgsConstructor
@Setter
@Getter
@Builder
public class CorreiosAltDTO {
    private Boolean erro;
    private String mensagem;
    private Integer total;
    private Dados dados;


    @AllArgsConstructor
    @NoArgsConstructor
    @Setter
    @Getter
    @Builder
    public static class Dados {
        private String uf;
        private String localidade;
        private String locNoSem;
        private String locNu;
        private String localidadeSubordinada;
        private String logradouroDNEC;
        private String logradouroTextoAdicional;
        private String logradouroTexto;
        private String bairro;
        private String baiNu;
        private String nomeUnidade;
        private String cep;
        private String tipoCep;
        private String numeroLocalidade;
        private String situacao;
        private String faixasCaixaPostal;
        private String faixasCep;
    }
}

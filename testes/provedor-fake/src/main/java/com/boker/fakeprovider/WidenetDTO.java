package com.boker.fakeprovider;

import lombok.*;

@AllArgsConstructor
@NoArgsConstructor
@Setter
@Getter
@Builder
public class WidenetDTO {
    private String code;
    private String state;
    private String city;
    private String district;
    private String address;
}

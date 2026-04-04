"use client"

import {
    createSystem,
    defaultConfig,
    defineConfig,
} from "@chakra-ui/react"


const config = defineConfig({
    theme: {
        tokens: {
            colors: {
                brand: {
                    DEFAULT: { value: "oklch(0.6   0.104 184.69)" },
                    50: { value: "oklch(0.959 0.059 184.36)" },
                    100: { value: "oklch(0.905 0.147 184.73)" },
                    200: { value: "oklch(0.827 0.143 184.65)" },
                    300: { value: "oklch(0.75  0.129 185.21)" },
                    400: { value: "oklch(0.676 0.117 184.64)" },
                    500: { value: "oklch(0.6   0.104 184.69)" },
                    600: { value: "oklch(0.506 0.088 184.4)" },
                    700: { value: "oklch(0.414 0.072 184.13)" },
                    800: { value: "oklch(0.321 0.055 185.56)" },
                    900: { value: "oklch(0.227 0.039 186.19)" },
                },

                primary: { value: "teal" },


                secondary: { value: "yellow" },
                danger: {
                    DEFAULT: { value: "red" },
                    500: { value: "red" },
                },
                info: { value: "blue" },
                warning: { value: "orange" },
                success: { value: "green" },

            }
        },
        semanticTokens: {
            colors: {
                brand: {
                    solid: { value: "{colors.brand.500}" },
                    contrast: { value: "{colors.brand.100}" },
                    fg: { value: "{colors.brand.400}" },
                    muted: { value: "{colors.brand.100}" },
                    subtle: { value: "{colors.brand.200}" },
                    emphasized: { value: "{colors.brand.300}" },
                    focusRing: { value: "{colors.brand.500}" },
                }
            }
        }
    }
})


export default createSystem(defaultConfig, config)
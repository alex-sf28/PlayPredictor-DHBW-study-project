import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
    input: "http://localhost:5013/swagger/v1/swagger.json", // sign up at app.heyapi.dev
    output: 'src/client',
    plugins: [
        '@hey-api/sdk',
        {
            enums: 'javascript',
            name: '@hey-api/typescript',
        },
    ]
});
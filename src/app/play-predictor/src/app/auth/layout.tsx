import { Box, Heading } from "@chakra-ui/react"

export default function AuthLayout({
    children
}: {
    children: React.ReactNode
}) {
    return (

        <Box
            display="flex"
            flexDirection="column"
            alignItems="center"
            justifyContent="center"
            gap="1rem"
            height="100%"

        >
            <Heading size="5xl" >PlayPredictor</Heading>
            <Box p="4"
                borderWidth="1px"
                minWidth="20rem"
            >
                {children}
            </Box>
        </Box>

    )

}
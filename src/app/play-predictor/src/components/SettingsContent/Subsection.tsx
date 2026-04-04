import { Box, Heading } from "@chakra-ui/react";

export default function Subsection({ children, heading }: { children: React.ReactNode, heading: string }) {

    return (<Box>
        <Heading size="md">{heading}</Heading>
        <Box
            marginLeft='0.2rem'
            fontSize="sm"
            color="GrayText"
        >
            {children}
        </Box>
    </Box>)
}
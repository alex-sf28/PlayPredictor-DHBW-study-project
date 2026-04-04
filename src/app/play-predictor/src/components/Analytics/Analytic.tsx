import { Box, Heading } from "@chakra-ui/react";

interface AnalyticProps {
    children: React.ReactNode;
    heading: string;
    description: string;
}


export default function Analytic({ children, heading, description }: AnalyticProps) {


    return (
        <Box>
            <Heading size="2xl" mb={2}>{heading}</Heading>
            <Heading size="md" mb={4} color="gray.500">{description}</Heading>
            <Box padding={3} spaceY={8}>
                {children}
            </Box>
        </Box>
    )
}
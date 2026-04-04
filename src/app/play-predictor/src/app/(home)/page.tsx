import { Button, Flex, Heading, Stack } from "@chakra-ui/react";

export default function Home() {

    return (
        <>
            <Flex alignContent="center" fontSize="2xl" height="100%" >
                <Stack marginX="auto" alignContent="center" height="100%" gapY={50}>
                    <Heading size="4xl" color="brand">Analyze your Match data!</Heading>
                    <Button marginX='auto' width={60} fontWeight="bold" fontSize="large" size="xl" asChild>
                        <a href="/setup">Get started</a>
                    </Button>
                </Stack>
            </Flex>

        </>
    );
}
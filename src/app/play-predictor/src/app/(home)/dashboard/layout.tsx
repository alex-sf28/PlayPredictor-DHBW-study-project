import { Flex, Stack } from "@chakra-ui/react";

export default function dashboardLayout({ children }: { children: React.ReactNode }) {

    return (
        <Flex alignContent="center" fontSize="2xl" height="100%" >
            <Stack marginX="auto" alignContent="center" height="100%">
                {children}
            </Stack>
        </Flex>
    )
}
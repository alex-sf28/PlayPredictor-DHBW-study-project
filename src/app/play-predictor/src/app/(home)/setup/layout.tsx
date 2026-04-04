"use client"

import { ReloadProvider } from "@/context/reload";
import { Box, Heading, Stack } from "@chakra-ui/react";


export default function SetupLayout({ children }: { children: React.ReactNode }) {
    return (
        <Stack display="flex" justifyContent="center">

            <Heading margin="auto" size="5xl" color='brand'>Setting things up</Heading>
            <Box paddingY={6} display="flex" justifyContent="center">
                <ReloadProvider>
                    {children}
                </ReloadProvider>
            </Box>

        </Stack>
    );
}
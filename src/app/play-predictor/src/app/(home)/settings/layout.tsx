import AuthGuard from "@/components/AuthGuard/AuthGuard";
import SideWaysTabs from "@/components/SideWaysTabs/SideWaysTabs";
import { sideWaysTabFields } from "@/config/sideWaysTabsFields";
import { Box, Flex } from "@chakra-ui/react";



export default function SettingsLayout({ children }: { children: React.ReactNode }) {
    return (<>
        <AuthGuard>
            <Flex gap='1.5rem' marginX="10%" >
                <SideWaysTabs sideTabFields={sideWaysTabFields} />
                <Box width="100%">
                    {children}
                </Box>
            </Flex>
        </AuthGuard>
    </>)
}
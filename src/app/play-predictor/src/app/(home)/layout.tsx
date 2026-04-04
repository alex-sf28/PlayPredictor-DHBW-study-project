import CheckAuth from "@/components/CheckAuth/CheckAuth"
import Navbar from "@/components/Navbar/Navbar"
import { Box } from "@chakra-ui/react"

export default function HomeLayout({
    children
}: {
    children: React.ReactNode
}) {
    return (
        <Box width="100vw" display="flex" justifyContent="center" flexDirection="column" >
            <Navbar />
            <CheckAuth>
                <Box flex={1} justifyContent="center" padding='1rem'>
                    {children}
                </Box>
            </CheckAuth>
        </Box>
    )

}
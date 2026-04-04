import Login from "@/components/Auth/Login";
import { Box, Link } from "@chakra-ui/react";

export default function LoginPage() {

    return (
        <>
            <Login />
            <Box>Or Register <Link href="/auth/register" colorPalette="brand">Here</Link></Box>
        </>
    );
}

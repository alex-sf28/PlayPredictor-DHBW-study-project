import { useAuth } from "@/lib/authStore";
import { Button } from "@chakra-ui/react";


export default function MainDashboard() {

    const { user } = useAuth.getState()

    return (

        <>
            <p>Analyze your Match data!</p>
            <Button marginX='auto' width={60} asChild>
                <a href="">Get started</a>
            </Button>
        </>

    )
}
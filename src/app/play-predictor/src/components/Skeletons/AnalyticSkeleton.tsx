import { Stack, Skeleton } from "@chakra-ui/react";

export default function AnalyticSkeleton() {
    return (
        <Stack gap={4}>
            <Skeleton height="2rem" width="20%" />
            <Skeleton height="1rem" width="50%" />
            <Skeleton height="14rem" width="50%" mb={4} />
        </Stack>
    )
}
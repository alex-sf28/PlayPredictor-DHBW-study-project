import { Stack, Skeleton, SkeletonCircle, HStack } from "@chakra-ui/react";

export default function UserCellSkeleton() {
    return <Stack>
        <HStack gap={4}>
            <SkeletonCircle size="10" />
            <Skeleton width={200} height={4} />
        </HStack>
        <HStack gap={4}>
            <SkeletonCircle size="10" />
            <Skeleton width={150} height={4} />
        </HStack>
    </Stack>
}
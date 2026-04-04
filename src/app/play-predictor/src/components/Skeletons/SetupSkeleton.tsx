import { Avatar, Flex, Skeleton, SkeletonCircle, SkeletonText, Stack } from "@chakra-ui/react";


export default function SetupSkeleton({ stepCount }: { stepCount: number }) {

    return (
        <Stack>
            <Flex justifyContent="space-between" flexDirection="row" alignItems="center" gapX={1} marginBottom={20}>
                <SkeletonCircle loading size={10}>
                    <Avatar.Root>
                    </Avatar.Root>
                </SkeletonCircle>
                {[...Array(stepCount > 1 ? stepCount - 1 : 0)].map((_, index) => (
                    <>
                        <Skeleton height={1} flex={1} />
                        <SkeletonCircle loading size={10}>
                            <Avatar.Root>
                            </Avatar.Root>
                        </SkeletonCircle>
                    </>
                ))}
            </Flex>

            <SkeletonText />
        </Stack>
    )
}
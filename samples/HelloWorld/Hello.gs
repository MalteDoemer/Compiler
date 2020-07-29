func main() {
    
    if (IsValid(20) && IsValid(36) || IsValid(36) && IsValid(0)) 
    {
        println("sucess!")
    }

    input()
}

func IsValid(num: int): bool
{
    var res = num != 36

    print(num + " is ")
    println(res ? "valid" : "invalid")

    return res
}
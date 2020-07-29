func main() {
    var rnd = rand(0,100)

    var i = rnd % 10 == 0 ? "nice" : rnd == 36 ? "glich nice" : "nid so nice"
    println(rnd)
    println(i)

    input()
}
<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
    xmlns:closures="http://czoneapps.eastsussex.gov.uk/Czone.WebService.SchoolsInformation/"
    xmlns:atom="http://www.w3.org/2005/Atom"
 >
    <xsl:output method="xml" indent="yes"/>
    
    <!-- Require the current date in two formats: RFC 822 for display in RSS feed, and ISO 8601 for numeric comparison of dates. 
         Doing numeric comparison of dates because .NET only supports XPath 1.0 and XPath 1.0 doesn't understand dates. -->
    <xsl:param name="Rfc822Date" />
    <xsl:param name="Iso8601Date" />

    <!-- Links to XHTML version of closure info, and the current feed-->
    <xsl:param name="XhtmlVersionUrl" />
    <xsl:param name="CurrentUrl" />
    
    <!-- Optionally limit feed to a single service rather than all services of a given type -->
    <xsl:param name="ServiceCode" />

    <!-- Transform begins here, by writing out the header of the RSS feed. Assumes feed will be hosted on www.eastsussex.gov.uk -->
    <xsl:template match="/">
        <xsl:text disable-output-escaping="yes">&lt;?xml-stylesheet  type="text/xsl" href="http://www.eastsussex.gov.uk/masterpages/rss/display-as-html.xslt" ?&gt;
        &lt;?xml-stylesheet  type="text/css" href="http://www.eastsussex.gov.uk/css/rssfeed.cssx" ?&gt;</xsl:text>
        <rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom">
            <channel>
                <link>
                    <xsl:value-of select="$XhtmlVersionUrl"/>
                </link>
                <language>en-GB</language>
                <copyright>
                    <xsl:value-of select="substring($Iso8601Date,1,4)"  />
                    <xsl:text> East Sussex County Council</xsl:text>
                </copyright>
                <pubDate>
                    <xsl:value-of select="$Rfc822Date"/>
                </pubDate>
                <lastBuildDate>
                    <xsl:value-of select="$Rfc822Date"/>
                </lastBuildDate>
                <generator>East Sussex County Council at www.eastsussex.gov.uk</generator>
                <image>
                    <title>East Sussex County Council logo</title>
                    <url>http://www.eastsussex.gov.uk/rss/escc-logo.gif</url>
                    <width>90</width>
                    <height>65</height>
                    <link>
                        <xsl:value-of select="$XhtmlVersionUrl"/>
                    </link>
                </image>
                <rating>(pics-1.1 "http://www.icra.org/ratingsv02.html" comment "ICRAonline EN v2.0" l gen true for "http://www.eastsussex.gov.uk/" r (nz 1 vz 1 lz 1 oz 1 cz 1) "http://www.rsac.org/ratingsv01.html" l gen true for "http://www.eastsussex.gov.uk/" r (n 0 s 0 v 0 l 0))</rating>

                <!-- Add the URL of the current feed -->
                <xsl:if test="$CurrentUrl != ''">
                    <atom:link rel="self" type="application/rss+xml">
                        <xsl:attribute xmlns="http://www.w3.org/2001/XMLSchema" name="href">
                            <xsl:value-of select="$CurrentUrl"/>
                        </xsl:attribute>
                    </atom:link>
                </xsl:if>

                <!-- Now process the services: just one if a service code was supplied; otherwise all of them -->
                <xsl:choose>
                    <xsl:when test="$ServiceCode != ''">
                        <xsl:apply-templates select="closures:ClosureInfo/closures:Services/closures:Service[closures:Code=$ServiceCode]" />
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:apply-templates select="closures:ClosureInfo/closures:Services/closures:Service" />
                    </xsl:otherwise>
                </xsl:choose>

            </channel>
        </rss>
    </xsl:template>

    <!-- This template runs for each service -->
    <xsl:template match="closures:Service">
    <!-- Store data about the service in variables, so that it's still available while looping through closures -->
        <xsl:variable name="ServiceId" select="closures:Id"/>
        <xsl:variable name="ServiceName" select="closures:Name" />
        <xsl:variable name="ServiceTypeSingular" select="closures:Type/closures:SingularText"/>
        <xsl:variable name="ServiceTypePlural" select="closures:Type/closures:PluralText"/>
        <xsl:variable name="CurrentServiceCode" select="closures:Code"/>
      <xsl:variable name="ServiceUri" select="closures:LinkedDataUri" />
        
        <!-- Create two variables which can be used to convert a string to lowercase -->
        <xsl:variable name="Lowercase">abcdefghijklmnopqrstuvwxyz</xsl:variable>
        <xsl:variable name="Uppercase">ABCDEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>

        <!-- If we haven't written the title and description of the feed yet, do it now that we have the service data available -->
        <xsl:if test="position() = 1">
            <xsl:choose>
                <xsl:when test="$ServiceCode != ''">
                    <title>
                        Closures for <xsl:value-of select="$ServiceName"  /> - East Sussex County Council
                    </title>
                    <description>
                        Dates when <xsl:value-of select="$ServiceName"  /> in East Sussex will be closed, and the reasons for its closure.
                    </description>
                </xsl:when>
                <xsl:otherwise>
                    <title>
                        Closures for <xsl:value-of select="$ServiceTypePlural"  /> run by East Sussex County Council
                    </title>
                    <description>
                        Dates when <xsl:value-of select="$ServiceTypePlural"  /> run by East Sussex County Council will be closed, and the reasons for their closure.
                    </description>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:if>

        <!-- Loop though all closures which end on or after today.
             The query is comparing dates as numbers because .NET only supports XPath 1.0 which doesn't understand dates.
             It resolves to something like closures:Closures/closures:Closure[closures:EndDate >= 20090526]
         -->
        <xsl:for-each select="closures:Closures/closures:Closure[number(concat(substring(closures:EndDate,1,4), substring(closures:EndDate,6,2), substring(closures:EndDate,9,2)))>=concat(substring($Iso8601Date,1,4), substring($Iso8601Date,6,2), substring($Iso8601Date,9,2))]">
            <!-- Sort the closures by start date, then subsort by end date -->
            <xsl:sort data-type="text" order="ascending" select="closures:StartDate"/>
            <xsl:sort data-type="text" order="ascending" select="closures:EndDate"/>
            
            <!-- Write one RSS item for each closure-->
            <item>
            <!-- Title should be "xxx closed, start date to end date (reason)" -->
                <title>
                    <xsl:value-of select="$ServiceName"/>
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="translate(closures:Status,$Uppercase,$Lowercase)"/>
                    <xsl:text>, </xsl:text>
                    <xsl:call-template name="FormatDate">
                        <xsl:with-param name="Date" select="closures:StartDate" />
                    </xsl:call-template>
                    <xsl:if test="closures:StartDate != closures:EndDate">
                        <xsl:text> to </xsl:text>
                        <xsl:call-template name="FormatDate">
                            <xsl:with-param name="Date" select="closures:EndDate" />
                        </xsl:call-template>
                    </xsl:if>
                    <xsl:text> (</xsl:text>
                    <xsl:value-of select="closures:Reason/closures:Reason"/>
                    <xsl:text>)</xsl:text>
                </title>
                
                <!-- If there are notes, use them as the item description -->
                <xsl:if test="closures:Notes != ''">
                    <description>
                        <xsl:value-of select="closures:Notes" />
                    </description>
                </xsl:if>

                <!-- Include a link to the most relevant page available, because Firefox Live Bookmarks needs a link to work. -->
                <link>
                    <xsl:value-of select="$ServiceUri"/>
                </link>
                
                <!-- Use the date the closure was last updated as the date for the item. 0001-01-01T00:00:00 is DateTime.MinValue in C# -->
                <xsl:choose>
                    <xsl:when test="closures:DateModified != '0001-01-01T00:00:00'">
                        <pubDate><xsl:call-template name="FormatRFC822Date">
                            <xsl:with-param name="Date" select="closures:DateModified" />
                        </xsl:call-template></pubDate>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:if test="closures:DateAdded != '0001-01-01T00:00:00'">
                            <pubDate><xsl:call-template name="FormatRFC822Date">
                                <xsl:with-param name="Date" select="closures:DateAdded" />
                            </xsl:call-template></pubDate>
                        </xsl:if>
                    </xsl:otherwise>
                </xsl:choose>
                
                <!-- Use the linked data URI of the closure to identify it in the RSS feed -->
                <guid isPermaLink="false"><xsl:value-of select="closures:LinkedDataUri"/></guid>
                
                <!-- Add the URL of the current feed -->
                <xsl:if test="$CurrentUrl != ''">
                    <source>
                        <xsl:attribute xmlns="http://www.w3.org/2001/XMLSchema" name="url">
                            <xsl:value-of select="$CurrentUrl"/>
                        </xsl:attribute>
                    </source>
                </xsl:if>
                
                <!-- Add categories for any data on which the RSS feed might need to be filtered. This makes the data
                     machine-readable without having to parse the item titles. -->
                
                <!-- Offer the service id as a filter, with its service type to make sure it's unique -->
                <category>
                    <xsl:value-of select="concat(translate(substring($ServiceTypeSingular,1,1),$Lowercase,$Uppercase), substring($ServiceTypeSingular,2))"/>
                    <xsl:text> </xsl:text>
                    <xsl:value-of select="$ServiceId"/>
                </category>

                <!-- Offer the service code as a filter, with its service type to make sure it's unique -->
                <xsl:if test="$CurrentServiceCode != ''">
                    <category>
                        <xsl:value-of select="concat(translate(substring($ServiceTypeSingular,1,1),$Lowercase,$Uppercase), substring($ServiceTypeSingular,2))"/>
                        <xsl:text> code </xsl:text>
                        <xsl:value-of select="$CurrentServiceCode"/>
                    </category>
                </xsl:if>
                
                <!-- Offer the service name as a filter -->
                <category>
                    <xsl:value-of select="$ServiceName"/>
                </category>
                
                <!-- Offer the reason for closure as a filter (for example, to select all holidays)-->
                <category>
                    <xsl:value-of select="closures:Reason/closures:Reason"/>
                </category>
                
                <!-- Offer the status as a filter (for example, to exclude confirmations that a service is partly closed) -->
                <category>
                    <xsl:value-of select="closures:Status"/>
                </category>
                
                <!-- Offer a filter to select only emergency closures, excluding planned ones like holidays -->
                <xsl:if test="closures:Reason/closures:Emergency='true'">
                <category>Emergency closure</category>
                </xsl:if>
            </item>
        </xsl:for-each>
    </xsl:template>

    <!-- When presented with a machine-readable date, translate it into a human readable date matching our house style-->
    <xsl:template name="FormatDate">
        <xsl:param name="Date" />
        <xsl:choose>
            <xsl:when test="substring($Date,9,1) = '0'">
                <xsl:value-of select="substring($Date,10,1)" />
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="substring($Date,9,2)" />
            </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
        <xsl:choose>
            <xsl:when test="substring($Date,6,2) = '01'">January</xsl:when>
            <xsl:when test="substring($Date,6,2) = '02'">February</xsl:when>
            <xsl:when test="substring($Date,6,2) = '03'">March</xsl:when>
            <xsl:when test="substring($Date,6,2) = '04'">April</xsl:when>
            <xsl:when test="substring($Date,6,2) = '05'">May</xsl:when>
            <xsl:when test="substring($Date,6,2) = '06'">June</xsl:when>
            <xsl:when test="substring($Date,6,2) = '07'">July</xsl:when>
            <xsl:when test="substring($Date,6,2) = '08'">August</xsl:when>
            <xsl:when test="substring($Date,6,2) = '09'">September</xsl:when>
            <xsl:when test="substring($Date,6,2) = '10'">October</xsl:when>
            <xsl:when test="substring($Date,6,2) = '11'">November</xsl:when>
            <xsl:when test="substring($Date,6,2) = '12'">December</xsl:when>
        </xsl:choose>
        <xsl:text> </xsl:text>
        <xsl:value-of select="substring($Date,1,4)"/>
    </xsl:template>
    
    
    <!-- When presented with a ISO8601 date, translate it into an RFC822 date-->
    <xsl:template name="FormatRFC822Date">
        <xsl:param name="Date" />
        <xsl:choose>
            <xsl:when test="substring($Date,9,1) = '0'">
                <xsl:value-of select="substring($Date,10,1)" />
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="substring($Date,9,2)" />
            </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
        <xsl:choose>
            <xsl:when test="substring($Date,6,2) = '01'">Jan</xsl:when>
            <xsl:when test="substring($Date,6,2) = '02'">Feb</xsl:when>
            <xsl:when test="substring($Date,6,2) = '03'">Mar</xsl:when>
            <xsl:when test="substring($Date,6,2) = '04'">Apr</xsl:when>
            <xsl:when test="substring($Date,6,2) = '05'">May</xsl:when>
            <xsl:when test="substring($Date,6,2) = '06'">Jun</xsl:when>
            <xsl:when test="substring($Date,6,2) = '07'">Jul</xsl:when>
            <xsl:when test="substring($Date,6,2) = '08'">Aug</xsl:when>
            <xsl:when test="substring($Date,6,2) = '09'">Sep</xsl:when>
            <xsl:when test="substring($Date,6,2) = '10'">Oct</xsl:when>
            <xsl:when test="substring($Date,6,2) = '11'">Nov</xsl:when>
            <xsl:when test="substring($Date,6,2) = '12'">Dec</xsl:when>
        </xsl:choose>
        <xsl:text> </xsl:text>
        <xsl:value-of select="substring($Date,1,4)"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="substring($Date,12,8)" />
        <xsl:text> UT</xsl:text>
    </xsl:template>
</xsl:stylesheet>
